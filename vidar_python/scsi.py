import struct

# Constants
CBW_SIGNATURE = 0x43425355  # 'USBC' in little-endian
CBW_TAG = 0x00000001        # Arbitrary tag, incremented with each command
CBW_DATA_TRANSFER_LENGTH = 52  # Data transfer length (as in the dump)
# Direction IN (0x80 means data from device to host)
CBW_FLAGS = 0x80
CBW_LUN = 0x00              # Logical Unit Number (0x00 in the dump)
# Command Descriptor Block length (SCSI command length)
CBW_CDB_LENGTH = 0x06

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
    return struct.pack(
        # Little-endian: 4-byte signature, 4-byte tag, 4-byte transfer length, 1-byte flags, etc.
        '<IIIBBB16s',
        CBW_SIGNATURE,
        CBW_TAG,
        CBW_DATA_TRANSFER_LENGTH,
        CBW_FLAGS,
        CBW_LUN,
        CBW_CDB_LENGTH,
        scsi_command_bytes  # Now 16 bytes, exactly as expected
    )

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
