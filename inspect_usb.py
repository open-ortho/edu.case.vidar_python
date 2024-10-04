import pyshark

# Path to the pcapng file
pcap_file = './Info.pcapng'  # Replace with the actual path

# Load the capture file
cap = pyshark.FileCapture(pcap_file)

# Function to extract the OUT data payloads from USB bulk transfers
def extract_data_out_from_usb(cap):
    data_out_list = []
    for packet in cap:
        # Filter for USB bulk transfers with OUT direction
        if 'USB' in packet:
            try:
                if packet.usb.endpoint_direction == 'OUT' and 'URB_BULK' in packet.usb.transfer_type:
                    # Extract the payload data (if present)
                    if hasattr(packet.usb, 'data'):
                        data_out_list.append(packet.usb.data)
            except AttributeError:
                continue
    return data_out_list

# Extract data_out from the capture
data_out_payloads = extract_data_out_from_usb(cap)

# Print the extracted data_out values
for i, data_out in enumerate(data_out_payloads, 1):
    print(f"Data OUT Packet {i}: {data_out}")

# Close the capture
cap.close()
