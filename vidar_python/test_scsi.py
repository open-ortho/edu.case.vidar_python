import unittest
import json
from vidar_python import v_scsi

class TestScsi(unittest.TestCase):

    def test_build_inquiry(self):
        # Print each byte in hexadecimal format
        cbw = v_scsi.build_inquiry_cbd()
        cbw_hex_string = ' '.join(f"{byte:02x}" for byte in cbw)
        expected_result_hex_string = "55 53 42 43 01 00 00 00 34 00 00 00 80 00 06 12 00 00 00 34 00 00 00 00 00 00 00 00 00 00 00"
        self.assertTrue(cbw_hex_string in expected_result_hex_string)

    def test_inquiry_result(self):
        # Example use of the function:
        response_hex_string = "06 00 02 02 2f 00 01 11 56 49 44 41 52 20 20 20 56 58 52 2d 31 32 20 20 20 20 20 20 20 20 20 20 34 39 2e 37 70 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00"
        response_bytes = bytes.fromhex(response_hex_string.replace(" ", ""))
        parsed_data = v_scsi.parse_scsi_inquiry_response(response_bytes)
        print(json.dumps(parsed_data, indent=4))
        self.assertEqual(parsed_data.get("Vendor ID"),"VIDAR")
        self.assertEqual(parsed_data.get("Product ID"),"VXR-12")
