""" USB related stuff.

Keep clean from SCSI stuff.
"""
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