using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using JetBrains.Annotations;

namespace Vint.Core.Server.Game.Protocol;

/// <summary>
/// Implements proxy protocol v2 https://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
/// </summary>
[PublicAPI]
public static class ProxyProtocolV2 {
    readonly static byte[] SignatureTemplate = "\r\n\r\n\0\r\nQUIT\n"u8.ToArray();

    public static bool TryParse(Stream stream, [NotNullWhen(true)] out ProxyPayload? proxyPayload, [NotNullWhen(false)] out byte[]? usedData) {
        proxyPayload = null;
        usedData = null;

        Span<byte> headerBuffer = stackalloc byte[16];

        int headerRead = stream.Read(headerBuffer);
        if (headerRead < 16) {
            usedData = headerBuffer[..headerRead].ToArray();
            return false;
        }

        if (!SignatureTemplate.SequenceEqual(headerBuffer[..12]) ||
            !TryGetProtocolCommand(headerBuffer[12], out ProxyProtocolCommand command) ||
            !TryGetConnectionType(headerBuffer[13], out ProxyAddressFamily addressFamily, out ProxyTransportProtocol transportProtocol)) {
            usedData = headerBuffer.ToArray();
            return false;
        }

        ushort addressLength = BinaryPrimitives.ReadUInt16BigEndian(headerBuffer[14..16]);
        int addressSize = addressFamily switch {
            ProxyAddressFamily.Unspecified when command == ProxyProtocolCommand.Local => 0,
            ProxyAddressFamily.IPv4 => 4,
            ProxyAddressFamily.IPv6 => 16,
            ProxyAddressFamily.Unix => 108,
            _ => -1
        };

        if (addressSize == -1) {
            usedData = headerBuffer.ToArray();
            return false;
        }

        // (address (addressSize bytes) + port (2 bytes)) * 2 (source & destination)
        int minRequired = addressSize > 0 ? (addressSize + 2) * 2 : 0;
        if (command != ProxyProtocolCommand.Local && minRequired > addressLength) {
            usedData = [..headerBuffer];
            return false;
        }

        Span<byte> addressDataBuffer = new(new byte[addressLength]);

        int addressDataRead = stream.Read(addressDataBuffer);
        if (addressDataRead < addressLength) {
            usedData = [..headerBuffer, ..addressDataBuffer[..addressDataRead]];
            return false;
        }

        if (command == ProxyProtocolCommand.Local) {
            proxyPayload = new ProxyPayload(command, addressFamily, transportProtocol, IPAddress.Any, IPAddress.Any, "", "", 0, 0);
            return true;
        }

        try {
            Span<byte> sourceAddressBytes = addressDataBuffer[..addressSize];
            Span<byte> destinationAddressBytes = addressDataBuffer.Slice(addressSize, addressSize);

            int portOffset = addressSize * 2;
            ushort sourcePort = BinaryPrimitives.ReadUInt16BigEndian(addressDataBuffer.Slice(portOffset, 2));
            ushort destinationPort = BinaryPrimitives.ReadUInt16BigEndian(addressDataBuffer.Slice(portOffset + 2, 2));

            IPAddress sourceAddress = IPAddress.Any;
            IPAddress destinationAddress = IPAddress.Any;
            string sourceUnixPath = "";
            string destinationUnixPath = "";

            if (addressFamily == ProxyAddressFamily.Unix) {
                sourceUnixPath = Encoding.ASCII.GetString(sourceAddressBytes);
                destinationUnixPath = Encoding.ASCII.GetString(destinationAddressBytes);
            } else {
                sourceAddress = new IPAddress(sourceAddressBytes);
                destinationAddress = new IPAddress(destinationAddressBytes);
            }

            proxyPayload = new ProxyPayload(command,
                addressFamily,
                transportProtocol,
                sourceAddress,
                destinationAddress,
                sourceUnixPath,
                destinationUnixPath,
                sourcePort,
                destinationPort);
            return true;
        } catch {
            usedData = [..headerBuffer, ..addressDataBuffer];
            return false;
        }
    }

    static bool TryGetProtocolCommand(byte versionAndCommand, out ProxyProtocolCommand command) {
        const byte protocolVersion = 0x20;
        command = ProxyProtocolCommand.Invalid;

        if ((versionAndCommand & 0xF0) != protocolVersion)
            return false;

        command = (versionAndCommand & 0x0F) switch {
            0x00 => ProxyProtocolCommand.Local,
            0x01 => ProxyProtocolCommand.Proxy,
            _ => ProxyProtocolCommand.Invalid,
        };

        return command != ProxyProtocolCommand.Invalid;
    }

    static bool TryGetConnectionType(
        byte addressFamilyAndTransportProtocol,
        out ProxyAddressFamily addressFamily,
        out ProxyTransportProtocol transportProtocol) {
        addressFamily = (addressFamilyAndTransportProtocol & 0xF0) switch {
            0x00 => ProxyAddressFamily.Unspecified,
            0x10 => ProxyAddressFamily.IPv4,
            0x20 => ProxyAddressFamily.IPv6,
            0x30 => ProxyAddressFamily.Unix,
            _ => ProxyAddressFamily.Invalid
        };

        transportProtocol = (addressFamilyAndTransportProtocol & 0x0F) switch {
            0x00 => ProxyTransportProtocol.Unspecified,
            0x01 => ProxyTransportProtocol.Stream,
            0x02 => ProxyTransportProtocol.Datagram,
            _ => ProxyTransportProtocol.Invalid
        };

        return addressFamily != ProxyAddressFamily.Invalid &&
               transportProtocol != ProxyTransportProtocol.Invalid;
    }
}

[PublicAPI]
public enum ProxyProtocolCommand : byte {
    Proxy = 0x00,
    Local = 0x01,
    Invalid = 0xFF
}

[PublicAPI]
public enum ProxyAddressFamily : byte {
    Unspecified = 0x00,
    IPv4 = 0x10,
    IPv6 = 0x20,
    Unix = 0x30,
    Invalid = 0xFF
}

[PublicAPI]
public enum ProxyTransportProtocol : byte {
    Unspecified = 0x00,
    Stream = 0x01,
    Datagram = 0x02,
    Invalid = 0xFF
}

[PublicAPI]
public record ProxyPayload(
    ProxyProtocolCommand ProtocolCommand,
    ProxyAddressFamily AddressFamily,
    ProxyTransportProtocol TransportProtocol,
    IPAddress SourceAddress,
    IPAddress DestinationAddress,
    string SourceUnixPath,
    string DestinationUnixPath,
    ushort SourcePort,
    ushort DestinationPort
);
