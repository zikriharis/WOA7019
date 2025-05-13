import requests
import base64

image_path = "fridge.jpg"
cloud_function_url = "https://us-central1-woa7019.cloudfunctions.net/process-image-text"

try:
    with open(image_path, "rb") as image_file:
        encoded_string = base64.b64encode(image_file.read()).decode('utf-8')

    payload = {
        "image_base64": encoded_string
    }

    headers = {
        'Content-Type': 'application/json'
    }

    response = requests.post(cloud_function_url, headers=headers, json=payload)
    response.raise_for_status()  # Raise an exception for bad status codes

    print("Cloud Function Response:")
    print(response.json())

except FileNotFoundError:
    print(f"Error: The file '{image_path}' was not found.")
except requests.exceptions.RequestException as e:
    print(f"Error sending request to Cloud Function: {e}")
except Exception as e:
    print(f"An unexpected error occurred: {e}")