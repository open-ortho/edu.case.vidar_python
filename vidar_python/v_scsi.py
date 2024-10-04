""" SCSI related stuff. 

Keep lean from USB stuff.
"""

from vidar_python.v_usb import build_cbw

# SCSI Inquiry Command (from the dump)
SCSI_INQUIRY_CDB = [
    0x12,  # Opcode for SCSI Inquiry
    0x00,  # CMDT, EVPD flags
    0x00,  # Reserved
    0x00,  # Reserved
    52,    # Allocation length (from the dump)
    0x00   # Control byte (default)
]

# Convert SCSI command to bytes
scsi_command_bytes = bytes(SCSI_INQUIRY_CDB) + \
    b'\x00' * (16 - len(SCSI_INQUIRY_CDB))


def build_inquiry_cbd():
    return build_cbw() + scsi_command_bytes

def parse_scsi_inquiry_response(response):
    # Create a dictionary to store the inquiry data
    inquiry_data = {
        "Peripheral Device Type": response[0] & 0x1f,
        "Removable Media": bool(response[1] & 0x80),
        "Version": response[2],  # Version of the standard
        "Response Data Format": response[3] & 0x0f,
        "Additional Length": response[4],
        "Vendor ID": response[8:16].decode().strip(),
        "Product ID": response[16:32].decode().strip(),
        "Product Revision Level": response[32:36].decode().strip()
    }
    return inquiry_data
