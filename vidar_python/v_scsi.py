"""
SCSI related stuff. 

Keep lean from USB stuff.
"""

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