using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public record AiAskRequest(string Question);
public record AiAskResponse(string Answer);

public class PerplexityService(AppDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory)
{
    private static readonly string SystemPrompt = """
        You are an analytics assistant for an order management system.
        Given a user question, identify the intent and return a JSON response ONLY with this structure:
        {"intent": "<intent_name>", "params": {<optional_params>}}

        Available intents:
        - count_orders_today: Count orders created today
        - count_by_status: Count orders grouped by status (or for a specific status in params.status)
        - avg_processing_time: Average time from Pendente to Finalizado in minutes
        - total_value_by_period: Total value of Finalizado orders. Optional params: period = "today"|"month"|"all"
        - list_recent: List 10 most recent orders

        Respond ONLY with valid JSON, nothing else.
        """;

    public async Task<AiAskResponse> AskAsync(string question)
    {
        var apiKey = config["Perplexity:ApiKey"] ?? throw new InvalidOperationException("Perplexity API key not configured");
        var model = config["Perplexity:Model"] ?? "sonar";

        var intent = await ExtractIntentAsync(question, apiKey, model);
        var data = await ExecuteQueryAsync(intent);
        var answer = await FormatAnswerAsync(question, data, apiKey, model);

        return new AiAskResponse(answer);
    }

    private async Task<JsonElement> ExtractIntentAsync(string question, string apiKey, string model)
    {
        var body = JsonSerializer.Serialize(new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = question }
            },
            max_tokens = 200,
            temperature = 0.1
        });

        using var http = httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.perplexity.ai/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var res = await http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json).RootElement;
        var content = parsed.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";

        return JsonDocument.Parse(content).RootElement;
    }

    private async Task<object> ExecuteQueryAsync(JsonElement intent)
    {
        var intentName = intent.TryGetProperty("intent", out var i) ? i.GetString() : null;
        var @params = intent.TryGetProperty("params", out var p) ? p : default;

        return intentName switch
        {
            "count_orders_today" => new
            {
                count = await db.Orders.CountAsync(o => o.DataCriacao.Date == DateTime.UtcNow.Date)
            },
            "count_by_status" when @params.ValueKind != JsonValueKind.Undefined
                && @params.TryGetProperty("status", out var s) => new
                {
                    status = s.GetString(),
                    count = await db.Orders.CountAsync(o => o.Status.ToString() == s.GetString())
                },
            "count_by_status" => (object)await db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { status = g.Key.ToString(), count = g.Count() })
                .ToListAsync(),
            "avg_processing_time" => new
            {
                avgMinutes = (await db.Orders
                    .Where(o => o.Status == OrderStatus.Finalizado)
                    .Select(o => o.DataCriacao)
                    .ToListAsync())
                    .DefaultIfEmpty()
                    .Average(d => (DateTime.UtcNow - d).TotalMinutes)
            },
            "total_value_by_period" => new
            {
                total = await db.Orders
                    .Where(o => o.Status == OrderStatus.Finalizado)
                    .SumAsync(o => (decimal?)o.Valor) ?? 0
            },
            "list_recent" => (object)await db.Orders
                .OrderByDescending(o => o.DataCriacao)
                .Take(10)
                .Select(o => new { o.Id, o.Cliente, o.Produto, o.Valor, status = o.Status.ToString(), o.DataCriacao })
                .ToListAsync(),
            _ => new { message = "Não entendi a pergunta. Tente perguntar sobre pedidos, valores ou status." }
        };
    }

    private async Task<string> FormatAnswerAsync(string question, object data, string apiKey, string model)
    {
        var dataJson = JsonSerializer.Serialize(data);
        var body = JsonSerializer.Serialize(new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "Você é um assistente amigável. Responda em português de forma clara e concisa usando os dados fornecidos." },
                new { role = "user", content = $"Pergunta: {question}\n\nDados: {dataJson}\n\nFormule uma resposta amigável em português." }
            },
            max_tokens = 300,
            temperature = 0.3
        });

        using var http = httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.perplexity.ai/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var res = await http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(json).RootElement;
        return parsed.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Não foi possível processar sua pergunta.";
    }
}
