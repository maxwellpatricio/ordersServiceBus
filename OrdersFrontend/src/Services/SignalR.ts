import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr'

let connection: HubConnection | null = null

export function getSignalRConnection(): HubConnection {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl('/hubs/orders')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()
  }
  return connection
}

export async function startSignalR(): Promise<HubConnection> {
  const conn = getSignalRConnection()
  if (conn.state === 'Disconnected') {
    await conn.start()
  }
  return conn
}
