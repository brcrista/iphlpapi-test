#include <cstdlib>
#include <iostream>
#include <string>

#include <Windows.h>
#include <iphlpapi.h>

using namespace std;

string to_tcp_state(DWORD connection_state)
{
    switch (connection_state)
    {
    case 1: return "CLOSED";
    case 2: return "LISTEN";
    case 3: return "SYN-SENT";
    case 4: return "SYN-RECEIVED";
    case 5: return "ESTABLISHED";
    case 6: return "FIN-WAIT-1";
    case 7: return "FIN-WAIT-2";
    case 8: return "CLOSE-WAIT";
    case 9: return "CLOSING";
    case 10: return "LAST-ACK";
    case 11: return "TIME-WAIT";
    case 12: return "DELETE TCB";
    default: return "UNKNOWN (" + to_string(connection_state) + ")";
    }
}

void print_tcp_connection(const MIB_TCPROW2& connection)
{
    cout
        << "Local address: "
        << connection.dwLocalAddr << ":" << connection.dwLocalPort << " "
        << "Remote address: "
        << connection.dwRemoteAddr << ":" << connection.dwRemotePort << " "
        << "State: "
        << to_tcp_state(connection.dwState)
        << endl;
}

int main()
{
    ULONG status;
    ULONG buffer_size = 0;

    // Call for the size
    status = ::GetTcpTable2(
        nullptr,
        &buffer_size,
        true);

    void* buffer = malloc(buffer_size);
    status = ::GetTcpTable2(
        reinterpret_cast<PMIB_TCPTABLE2>(buffer),
        &buffer_size,
        true);

    PMIB_TCPTABLE2 tcp_table_ipv4 = nullptr;

    switch (status)
    {
    case NO_ERROR:
        tcp_table_ipv4 = reinterpret_cast<PMIB_TCPTABLE2>(buffer);
        cout << "Found " << tcp_table_ipv4->dwNumEntries << " connections:" << endl;
        for (DWORD i = 0; i < tcp_table_ipv4->dwNumEntries; i++)
        {
            const auto row = tcp_table_ipv4->table[i];
            print_tcp_connection(row);
        }

        free(buffer);
        return 0;
    case ERROR_INSUFFICIENT_BUFFER:
        cerr << "Insufficient buffer: required " << buffer_size << " bytes." << endl;

        free(buffer);
        return 1;
    case ERROR_INVALID_PARAMETER:
        cerr << "Could not write to the buffer. Is it null?" << endl;

        free(buffer);
        return 1;
    case ERROR_NOT_SUPPORTED:
        cerr << "Not supported on this OS." << endl;

        free(buffer);
        return 1;
    default:
        cerr << "An unknown error occurred." << endl;

        free(buffer);
        return 1;
    }
}