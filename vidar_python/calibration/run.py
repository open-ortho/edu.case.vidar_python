import usb
from vidar_python import *
from vidar_python.v_usb import find_usb_endpoints
from vidar_python.debug_funcs import verbose_print

from vidar_python.calibration.create_recv_diag_packet import build_calibration_recv_diag_cbd
from vidar_python.calibration.create_send_diag_packet import build_calibration_send_diag_cbd
from vidar_python.calibration.create_write_packet import build_calibration_write_10_cbd
from vidar_python.calibration import *

"""
Runs the calibration sequence on a device.
"""
def run_calibration(device=None, scan_timeout=100, verbose=False):
    # Find the device
    dev = device
    if dev is None:
        dev = usb.core.find(idVendor=VENDOR_ID, idProduct=PRODUCT_ID)
    
    # if there is still no device, it cannot be found
    if dev is None:
        raise ValueError('Device not found')

    # Set the active configuration
    dev.reset()
    dev.set_configuration()

    # find the in and out endpoints for the device
    endpoint_in, endpoint_out = find_usb_endpoints(device=dev)
    verbose_print(endpoint_out, verbose)
    verbose_print(endpoint_in, verbose)



    # construct the SCSI commands that will be written to the device
    write_cbw = build_calibration_write_10_cbd()
    send_diag = build_calibration_send_diag_cbd()
    recv_diag = build_calibration_recv_diag_cbd()


    # Send the CBW (OUT direction)
    dev.write(endpoint_out, write_cbw)

    # write commamnd for calibration to the scanner.
    dev.write(endpoint_out, CALIBRATION_WRITE_PAYLOAD)

    verbose_print(dev.read(endpoint_in, DEVICE_ALLOCATION_LENGTH), verbose)



    # Send the CBW (OUT direction)
    dev.write(endpoint_out, send_diag)

    # write commamnd for calibration to the scanner.
    dev.write(endpoint_out, CALIBRATION_SEND_DIAGNOSTIC_PAYLOAD)

    verbose_print(dev.read(endpoint_in, DEVICE_ALLOCATION_LENGTH), verbose)



    # Send the CBW (OUT direction)
    dev.write(endpoint_out, recv_diag)

    # wait at least scan_timeout seconds for calibration to happen.
    # tends to be ~60s
    verbose_print(dev.read(endpoint_in, DEVICE_ALLOCATION_LENGTH, timeout=scan_timeout*1000), verbose)

    verbose_print(dev.read(endpoint_in, DEVICE_ALLOCATION_LENGTH), verbose)

    dev.reset()
    usb.util.release_interface(dev, 0)

    verbose_print("\n" + "-"*50 + "\nCalibration Complete\n" + "-"*50, verbose)