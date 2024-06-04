from socket import socket


class Client:
    def __init__(self, _socket : socket) -> None:
        self.__socket = _socket

    @property
    def socket(self):
        return self.__socket