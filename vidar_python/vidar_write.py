import usb.core
import usb.util
from vidar_python.v_scsi import build_recieve_diagnostic_cbd, build_send_diagnostic_cbd
from vidar_python.v_usb import find_usb_endpoints

from vidar_python import VENDOR_ID, PRODUCT_ID
from vidar_python.SCSI_commands import write_10


# Find the device
dev = usb.core.find(idVendor=VENDOR_ID, idProduct=PRODUCT_ID)
if dev is None:
    raise ValueError('Device not found')

# Set the active configuration
dev.set_configuration()

endpoint_in, endpoint_out = find_usb_endpoints(device=dev)


write_cbw = write_10
send_diag = build_send_diagnostic_cbd()
recv_diag = build_recieve_diagnostic_cbd()


# Send the CBW (OUT direction)
dev.write(endpoint_out, write_cbw)

# write commamnd for calibration to the scanner.
dev.write(endpoint_out, bytes([0x00, 0x00]))

dev.read(endpoint_in, 512)



# Send the CBW (OUT direction)
dev.write(endpoint_out, send_diag)

# write commamnd for calibration to the scanner.
dev.write(endpoint_out, bytes([0x82, 0x00, 0x00, 0x01, 0x00]))

dev.read(endpoint_in, 512)



# Send the CBW (OUT direction)
dev.write(endpoint_out, recv_diag)

# wait at least 100 seconds for calibration to happen.
dev.read(endpoint_in, 512, timeout=100*1000)

dev.read(endpoint_in, 512)

# Optionally, clear the halt condition on the IN endpoint (if required)
dev.clear_halt(endpoint_in)
# when using libusbK backend, you cannot close the device, you release the interface.
#dev.close()

usb.util.release_interface(dev, 0)

dev.reset()