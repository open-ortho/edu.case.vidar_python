# Constants
import struct
from vidar_python import LUN


CBW_SIGNATURE = 0x43425355  # 'USBC' in little-endian
CBW_TAG = 0x00000001        # Arbitrary tag, incremented with each command
CBW_DATA_TRANSFER_LENGTH = 2  # Data transfer length (as in the dump)
# Direction IN (0x80 means data from device to host)
CBW_FLAGS = 0x80
CBW_LUN = LUN
# Command Descriptor Block length (SCSI command length)
CBW_CDB_LENGTH = 0x0A

LBA = 0x0100001a  # Logical Block Address (32 bits)
transfer_length = 0x0010  # Transfer length (16 bits)
data = b"Your data to be written"  # Data you want to write (must match transfer length)

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

def build_cbd():
    return build_cbw() + build_scsi_command_bytes()

def build_scsi_command_bytes():
    cdb = [
        0x2A,  # Opcode for SCSI Inquiry
        0x00,   # Flags
        0x01,  # Start LDA
        0x00,
        0x00,
        0x1a,  # end LDA
        0x00,  # reserved / group number
        0x00,  # transfer length begin
        0x02,  # transfer length end
        0x00,  # control
    ]
    return bytes(cdb) + b'\x00' * (16 - len(cdb))

write_10 = build_cbd()