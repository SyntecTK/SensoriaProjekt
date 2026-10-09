using System;
using System.Runtime.InteropServices;

public static class PSMoveNative
{
    const string Dll = "psmoveapi";   // = psmoveapi.dll

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool psmove_init(int version);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int psmove_count_connected();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr psmove_connect_by_id(int id);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void psmove_set_leds(IntPtr move, byte r, byte g, byte b);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int psmove_update_leds(IntPtr move);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void psmove_disconnect(IntPtr move);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr psmove_get_serial(IntPtr move);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int psmove_connection_type(IntPtr move);
}