import ctypes

my_dll = ctypes.WinDLL("C:\Projects\edu.case.vidar_python\dll_python\Vscsi32.dll")

result = my_dll.Calibrate()

print(result)