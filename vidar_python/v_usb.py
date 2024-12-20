"""
USB related stuff.

Keep clean from SCSI stuff.
"""
import struct
import usb
from vidar_python import LUN, USB_CBW_FLAGS, USB_CBW_SIGNATURE

# Need a class for cbd_packaging to keep track of the tags sent in past CBWs
class CBD_packager():
    # Tag that increments for each command sent
    USB_CBW_TAG = 0x00000001 

    """
    Package the USB CBW and the SCSI CDB into the CBD
    """
    @staticmethod
    def create_package_cbd(scsi_command_bytes, data_transfer_length):
        cdb_len = len(scsi_command_bytes)
        
        cbw = struct.pack(
            # Little-endian: 4-byte signature, 4-byte tag, 4-byte transfer length, 1-byte flags, etc.
            '<IIIBBB',
            USB_CBW_SIGNATURE,
            CBD_packager.USB_CBW_TAG,
            data_transfer_length,
            USB_CBW_FLAGS,
            LUN,
            cdb_len,
        )

        padded_cdb = (scsi_command_bytes + b'\x00' * (16 - cdb_len))
        
        # Increment the command tag.
        CBD_packager.USB_CBW_TAG += 0x1

        return cbw + padded_cdb

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