import usb.core
import usb.util
from vidar_python.v_scsi import build_inquiry_cbd

from vidar_python import VENDOR_ID, PRODUCT_ID

# Find the device
dev = usb.core.find(idVendor=VENDOR_ID, idProduct=PRODUCT_ID)
if dev is None:
    raise ValueError('Device not found')

# Set the active configuration
dev.set_configuration()

# Define the OUT endpoint and IN endpoint (adjust as needed based on your USB dump)
endpoint_out = 0x01  # OUT endpoint (from the dump)
endpoint_in = 0x82   # IN endpoint (from the dump)

cbw = build_inquiry_cbd()

# Send the CBW (OUT direction)
dev.write(endpoint_out, cbw)

# Read the response from the device (IN direction)
try:
    response = dev.read(endpoint_in, 52, timeout=5000)  # Adjust size based on your expected response length
    print("Response:", response)
except usb.core.USBError as e:
    print("Error reading from device:", e)

# Optionally, clear the halt condition on the IN endpoint (if required)
dev.clear_halt(endpoint_in)
dev.close()
