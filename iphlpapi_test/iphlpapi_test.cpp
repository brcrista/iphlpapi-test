// iphlpapi_test.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"

#include <cstdlib>
#include <iostream>

#include <Windows.h>
#include <iphlpapi.h>

using namespace std;

void print_tcp_connection(const MIB_TCPROW2& connection)
{
    cout
        << "Local address: "
        << connection.dwLocalAddr << ":" << connection.dwLocalPort << " "
        << "Remote address: "
        << connection.dwRemoteAddr << ":" << connection.dwRemotePort << " "
        << "State: "
        << connection.dwState
        << endl;
}

int main()
{
    ULONG status;
    ULONG buffer_size = sizeof(MIB_TCPTABLE2);
    void* buffer = malloc(sizeof(MIB_TCPTABLE2));

    // Call for size
    status = ::GetTcpTable2(
        reinterpret_cast<PMIB_TCPTABLE2>(buffer),
        &buffer_size,
        TRUE);

    if (status == ERROR_INSUFFICIENT_BUFFER)
    {
        free(buffer);
        buffer = malloc(buffer_size);
    }
    else if (status != NO_ERROR)
    {
        cerr << "Something is wrong, we didn't get the size back. Status code: " << status << endl;
        free(buffer);
        return 1;
    }

    status = ::GetTcpTable2(
        reinterpret_cast<PMIB_TCPTABLE2>(buffer),
        &buffer_size,
        TRUE);

    PMIB_TCPTABLE2 tcp_table_ipv4 = NULL;

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