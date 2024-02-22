# server.py
import argparse
import asyncio
import websockets
import ssl
import random
from packet import Packet, Action

class Server:
    def __init__(self, host="0.0.0.0", port=9999, ssl_cert_path=None, ssl_key_path=None):
        self.host = host
        self.port = port
        self.ssl_context = None
        if ssl_cert_path and ssl_key_path:
            self.ssl_context = ssl.create_default_context(ssl.Purpose.CLIENT_AUTH)
            self.ssl_context.load_cert_chain(ssl_cert_path, ssl_key_path)
        self.connected_users = {}

    async def handler(self, websocket, path):
        user_id = random.randint(2, 2**31 - 1)
        try:
            while True:
                try:
                    message = await asyncio.wait_for(websocket.recv(), timeout=20)
                except asyncio.TimeoutError:
                    # No data in 20 seconds, check the connection.
                    try:
                        pong = await websocket.ping()
                        await asyncio.wait_for(pong, timeout=10)
                        continue
                    except:
                        # No pong received, or timeout
                        print(f"User {user_id} disconnected")
                        break
                # Process the message
                packet = Packet.from_bytes(message)
                print(f"Received packet: {packet}")

                if packet.action == Action.LOGIN:
                    self.connected_users[websocket] = packet.payloads[0]
                    print(f"User {user_id} logged in with username {packet.payloads[0]}")
                    await websocket.send(Packet(Action.SEND_USER_DATA, str(user_id)).to_bytes())
                    for ws in self.connected_users.keys():
                        if ws != websocket:
                            await ws.send(Packet(Action.ADD_USER, str(user_id), self.connected_users[websocket]).to_bytes())
                            await websocket.send(Packet(Action.ADD_USER, str(user_id), self.connected_users[ws]).to_bytes())

                elif packet.action == Action.CHAT:
                    print(f"User {user_id}:{packet.payloads[0]} sent chat message: {packet.payloads[1]}")
                    for ws in self.connected_users.keys():
                        await ws.send(packet.to_bytes())

        except Exception as e:
            print(f"User {user_id} disconnected due to error: {e}")
        finally:
            if websocket in self.connected_users:
                del self.connected_users[websocket]
                for ws in self.connected_users.keys():
                    await ws.send(Packet(Action.USER_DISCONNECTED, str(user_id)).to_bytes())

    def start(self):
        start_server = websockets.serve(self.handler, self.host, self.port, ssl=self.ssl_context)
        print(f"Server started on {self.host}:{self.port}")
        asyncio.get_event_loop().run_until_complete(start_server)
        asyncio.get_event_loop().run_forever()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Start the WebSocket server")
    parser.add_argument("--port", help="Port of the server")
    parser.add_argument("--cert", help="Path to SSL certificate")
    parser.add_argument("--key", help="Path to SSL key")
    args = parser.parse_args()

    server = Server(port=args.port, ssl_cert_path=args.cert, ssl_key_path=args.key)
    server.start()