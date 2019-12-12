#include <cstdlib>
#include <functional>
#include <iostream>
#include <string>

#include <Windows.h>
#include <iphlpapi.h>

// Exported from iphlpapi.dll, but is not in iphlpapi.h
extern "C" __declspec(dllimport) DWORD __stdcall InternalGetBoundTcpEndpointTable(
    PMIB_TCPTABLE2* ppTcpTable,
    HANDLE hHeap,
    DWORD dwFlags);

using namespace std;

string to_tcp_state(DWORD connection_state)
{
    // See tcpmib.h
    switch (connection_state)
    {
    case MIB_TCP_STATE_CLOSED: return "CLOSED";
    case MIB_TCP_STATE_LISTEN: return "LISTEN";
    case MIB_TCP_STATE_SYN_SENT: return "SYN-SENT";
    case MIB_TCP_STATE_SYN_RCVD: return "SYN-RECEIVED";
    case MIB_TCP_STATE_ESTAB: return "ESTABLISHED";
    case MIB_TCP_STATE_FIN_WAIT1: return "FIN-WAIT-1";
    case MIB_TCP_STATE_FIN_WAIT2: return "FIN-WAIT-2";
    case MIB_TCP_STATE_CLOSE_WAIT: return "CLOSE-WAIT";
    case MIB_TCP_STATE_CLOSING: return "CLOSING";
    case MIB_TCP_STATE_LAST_ACK: return "LAST-ACK";
    case MIB_TCP_STATE_TIME_WAIT: return "TIME-WAIT";
    case MIB_TCP_STATE_DELETE_TCB: return "DELETE TCB";
    default: return "UNKNOWN (" + to_string(connection_state) + ")";
    }
}

void print_tcp_connection(const MIB_TCPROW2& connection, const function<string(DWORD)>& get_tcp_state)
{
    cout
        << "Local address: "
        << connection.dwLocalAddr << ":" << connection.dwLocalPort << " "
        << "Remote address: "
        << connection.dwRemoteAddr << ":" << connection.dwRemotePort << " "
        << "State: "
        << get_tcp_state(connection.dwState)
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
        // Print the table of connnections
        tcp_table_ipv4 = reinterpret_cast<PMIB_TCPTABLE2>(buffer);
        cout << "Found " << tcp_table_ipv4->dwNumEntries << " connections:" << endl;
        for (DWORD i = 0; i < tcp_table_ipv4->dwNumEntries; i++)
        {
            const auto row = tcp_table_ipv4->table[i];
            print_tcp_connection(row, to_tcp_state);
        }
        break;
    case ERROR_INSUFFICIENT_BUFFER:
        cerr << "Insufficient buffer: required " << buffer_size << " bytes." << endl;
        break;
    case ERROR_INVALID_PARAMETER:
        cerr << "Could not write to the buffer. Is it null?" << endl;
        break;
    case ERROR_NOT_SUPPORTED:
        cerr << "Not supported on this OS." << endl;
        break;
    default:
        cerr << "An unknown error occurred." << endl;
        break;
    }

    free(buffer);
    if (status != NO_ERROR)
    {
        return 1;
    }


    PMIB_TCPTABLE2 tcp_table_bound;
    HANDLE heap = ::GetProcessHeap();

    status = ::InternalGetBoundTcpEndpointTable(
        &tcp_table_bound,
        heap,
        0);

    // Print the table of bound ports
    if (status == NO_ERROR)
    {
        cout << endl;

        cout << "Found " << tcp_table_bound->dwNumEntries << " bound ports:" << endl;
        for (DWORD i = 0; i < tcp_table_bound->dwNumEntries; i++)
        {
            const auto row = tcp_table_bound->table[i];
            print_tcp_connection(row, [] (DWORD _) { return "BOUND"; });
        }
    }
    else
    {
        cerr << "An unknown error occurred." << endl;
    }

    ::HeapFree(heap, 0, tcp_table_bound);
    if (status != NO_ERROR)
    {
        return 1;
    }

    return 0;
}