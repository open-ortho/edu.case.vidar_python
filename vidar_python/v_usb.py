""" USB related stuff.

Keep clean from SCSI stuff.
"""
import struct

import usb
from vidar_python import LUN

# Constants
CBW_SIGNATURE = 0x43425355  # 'USBC' in little-endian
CBW_TAG = 0x00000001        # Arbitrary tag, incremented with each command
CBW_DATA_TRANSFER_LENGTH = 52  # Data transfer length (as in the dump)
# Direction IN (0x80 means data from device to host)
CBW_FLAGS = 0x80
CBW_LUN = LUN
# Command Descriptor Block length (SCSI command length)
CBW_CDB_LENGTH = 0x06


def build_cbw():
    return struct.pack(
        # Little-endian: 4-byte signature, 4-byte tag, 4-byte transfer length, 1-byte flags, etc.
        '<IIIBBB',
        CBW_SIGNATURE,
        CBW_TAG,
        CBW_DATA_TRANSFER_LENGTH,
        CBW_FLAGS,
        CBW_LUN,
        CBW_CDB_LENGTH,
    )

"""
Given a pyusb device, return the IN and OUT endpoints.
"""
def find_usb_endpoints(device):
    in_endpoint, out_endpoint = None, None

    # check every endpoint in the devices interface to find IN and OUT
    for cfg in device:
        for intf in cfg:
            for ep in intf:
                if usb.util.endpoint_direction(ep.bEndpointAddress) == usb.util.ENDPOINT_IN:
                    in_endpoint = ep
                elif usb.util.endpoint_direction(ep.bEndpointAddress) == usb.util.ENDPOINT_OUT:
                    out_endpoint = ep

    if (in_endpoint is None) or (out_endpoint is None):
        print("One or both of the USB endpoints could not be found")
        print(f"IN: {in_endpoint}")
        print(f"OUT: {out_endpoint}")


    return (in_endpoint, out_endpoint)