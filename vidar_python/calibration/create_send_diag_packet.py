from vidar_python import *
from vidar_python.v_usb import create_package_cbd
from vidar_python.calibration import *


"""
Creates the SCSI command block for the calibration SEND_DIAGNOSTIC command
"""
def build_scsi_command_bytes():
    cdb = [
        SEND_DIAGNOSTIC_OP_CODE,
        0x00,  # CMDT, EVPD flags
        0x00,  # Reserved
        0x00,  # Reserved
        CALIBRATION_SEND_DIAGNOSTIC_ALLOCATION_LENGTH,
        CALIBRATION_CONTROL,
    ]
    return bytes(cdb)

"""
Creates the CBD SEND_DIAGNOSTIC packet for the calibration sequence.
"""
def build_calibration_send_diag_cbd():
    scsi_cdb = build_scsi_command_bytes()

    cbd = create_package_cbd(scsi_cdb, data_transfer_length=52)

    return cbd
