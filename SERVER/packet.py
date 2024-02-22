# packet.py
import json
from enum import Enum

class Action(Enum):
    LOGIN = "Login"
    SEND_USER_DATA = "SendUserData"
    USER_DISCONNECTED = "UserDisconnected"
    ADD_USER = "AddUser"
    CHAT = "Chat"
    PLAYER_TRANSFORM_SYNC = "PlayerTransformSync"

class Packet:
    def __init__(self, action, *payloads):
        self.action = action
        self.payloads = payloads

    def to_bytes(self):
        serialized_dict = {"a": self.action.value}
        for i, payload in enumerate(self.payloads):
            serialized_dict[f"p{i}"] = payload
        return json.dumps(serialized_dict).encode()

    @staticmethod
    def from_bytes(bytes_str):
        json_str = bytes_str.decode()
        obj_dict = json.loads(json_str)
        action = Action(obj_dict["a"])
        payloads = [value for key, value in obj_dict.items() if key.startswith("p")]
        return Packet(action, *payloads)