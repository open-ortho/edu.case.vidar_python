#include "vidar_config.h"
#include <stdio.h>
#include <libusb.h>
#include <string.h>

#define INQUIRY_CMD_LEN 6
#define CBW_LEN 31
#define BULK_ENDPOINT_OUT 0x01  // Replace with actual OUT endpoint
#define BULK_ENDPOINT_IN  0x82  // Replace with actual IN endpoint
#define TIMEOUT 5000

// SCSI Inquiry Command (opcode 0x12)
unsigned char scsi_inquiry_command[INQUIRY_CMD_LEN] = {
    0x12,  // Opcode for SCSI Inquiry
    0x00,  // CMDT, EVPD flags
    0x00,  // Reserved
    0x00,  // Reserved
    52,    // Allocation length (expected response length)
    0x00   // Control byte
};

// Command Block Wrapper (CBW) for USB Mass Storage
unsigned char cbw[CBW_LEN] = {
    0x55, 0x53, 0x42, 0x43,  // Signature: 'USBC'
    0x01, 0x00, 0x00, 0x00,  // Tag: Arbitrary (set to 1 here)
    0x34, 0x00, 0x00, 0x00,  // Data Transfer Length (52 bytes, little-endian)
    0x80,                    // Flags: Direction = IN (0x80)
    0x00,                    // LUN (Logical Unit Number)
    0x06,                    // Length of Command Descriptor Block (CDB)
};

int main(void) {
    libusb_device_handle *handle;
    libusb_context *ctx = NULL;
    int r;
    unsigned char response[52];  // Buffer to hold device response

    // Initialize libusb
    r = libusb_init(&ctx);
    if (r < 0) {
        printf("Failed to initialize libusb\n");
        return 1;
    }

    // Open the USB device
    handle = libusb_open_device_with_vid_pid(ctx, VENDOR_ID, PRODUCT_ID);
    if (handle == NULL) {
        printf("Device not found\n");
        libusb_exit(ctx);
        return 1;
    }

    // Set active configuration
    libusb_set_configuration(handle, 1);
    libusb_claim_interface(handle, 0);

    // Copy SCSI Inquiry Command into the CBW (starting from the 16th byte)
    memcpy(&cbw[15], scsi_inquiry_command, INQUIRY_CMD_LEN);

    // Send CBW to device (USB Bulk OUT transfer)
    r = libusb_bulk_transfer(handle, BULK_ENDPOINT_OUT, cbw, CBW_LEN, NULL, TIMEOUT);
    if (r < 0) {
        printf("Failed to send CBW: %s\n", libusb_error_name(r));
        libusb_close(handle);
        libusb_exit(ctx);
        return 1;
    }

    // Receive response from device (USB Bulk IN transfer)
    r = libusb_bulk_transfer(handle, BULK_ENDPOINT_IN, response, sizeof(response), NULL, TIMEOUT);
    if (r < 0) {
        printf("Failed to receive response: %s\n", libusb_error_name(r));
    } else {
        printf("Device response received successfully\n");
        for (int i = 0; i < sizeof(response); i++) {
            printf("%02x ", response[i]);
        }
        printf("\n");
    }

    // Cleanup
    libusb_release_interface(handle, 0);
    libusb_close(handle);
    libusb_exit(ctx);

    return 0;
}
