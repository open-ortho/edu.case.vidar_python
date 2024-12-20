from vidar_python import *
from vidar_python.v_usb import CBD_packager
from vidar_python.calibration import *


"""
Creates the SCSI command block for the calibration WRITE(10) command
"""
def build_scsi_command_bytes():
    cdb = [
        WRITE_10_OP_CODE,
        0x00, # Flags
        # logical block address to write to (4 bytes)
        CALIBRATION_WRITE_LBA[0],
        CALIBRATION_WRITE_LBA[1],
        CALIBRATION_WRITE_LBA[2],
        CALIBRATION_WRITE_LBA[3],
        CALIBRATION_WRITE_GROUP_NUMBER,
        # transfer length is 2 bytes
        CALIBRATION_WRITE_TRANSFER_LENGTH[0],
        CALIBRATION_WRITE_TRANSFER_LENGTH[1],
        CALIBRATION_CONTROL,
    ]
    return bytes(cdb)

"""
Creates the CBD Write(10) packet for the calibration sequence.
"""
def build_calibration_write_10_cbd():
    scsi_cdb = build_scsi_command_bytes()

    cbd = CBD_packager.create_package_cbd(scsi_cdb, data_transfer_length=2)

    return cbd
