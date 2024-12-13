""" SCSI related stuff. 

Keep lean from USB stuff.
"""
from vidar_python.v_usb import build_cbw

SCSI_TEST_UNIT_READY = 0x0
SCSI_INQUIRY = 0x12
SCSI_RECEIVE_DIAGNOSTIC_RESULTS = 0x1c
SCSI_SEND_DIAGNOSTIC = 0x1d

def build_scsi_command_bytes(command, l):
    cdb = [
        command,  # Opcode for SCSI Inquiry
        0x00,  # CMDT, EVPD flags
        0x00,  # Reserved
        0x00,  # Reserved
        l, # TODO change back,    # Allocation length (from the dump)
        0x00,   # Control byte (default)
    ]
    return bytes(cdb) + b'\x00' * (16 - len(cdb))


def build_cbd(command, l):
    return build_cbw() + build_scsi_command_bytes(command, l)

def build_inquiry_cbd():
    return build_cbd(SCSI_INQUIRY, 52)

def build_send_diagnostic_cbd():
    return build_cbd(SCSI_SEND_DIAGNOSTIC, 0x5)

def build_recieve_diagnostic_cbd():
    return build_cbd(SCSI_RECEIVE_DIAGNOSTIC_RESULTS, 0x85)

def parse_scsi_inquiry_response(response):
    # Create a dictionary to store the inquiry data
    inquiry_data = {
        "Peripheral Device Type": response[0] & 0x1f,
        "Removable Media": bool(response[1] & 0x80),
        "Version": response[2],  # Version of the standard
        "Response Data Format": response[3] & 0x0f,
        "Additional Length": response[4],
        "Vendor ID": response[8:16].strip(),
        "Product ID": response[16:32].strip(),
        "Product Revision Level": response[32:36].strip(),
    }
    return inquiry_data