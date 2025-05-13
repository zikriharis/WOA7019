import os
import json
import vertexai
from vertexai.preview.vision_models import Image, ImageTextModel
from flask import Request  # Assuming this is used in a Cloud Function

PROJECT_ID="woa7019"
REGION="us-central1"
QUESTION = "what is the object in the image? (choose from fridge, door, or desk)"

def vqa(image_bytes: bytes) -> str:
    vertexai.init(project=PROJECT_ID, location=REGION)

    model = ImageTextModel.from_pretrained("imagetext@001")
    source_img = Image(image_bytes=image_bytes)

    answers = model.ask_question(
        image=source_img, 
        question=QUESTION, 
        number_of_results=1
    )

    return answers[0].text.lower() if answers else None

def main(request: Request):
    request_json = request.get_json(silent=True)
    image_data = None

    if request_json and 'image_base64' in request_json:
        image_data = request_json['image_base64']
    elif request.files and 'image' in request.files:
        image_file = request.files['image']
        image_data = base64.b64encode(image_file.read()).decode('utf-8')
    else:
        return json.dumps({"error": "No image data provided"}), 400, {'Content-Type': 'application/json'}

    if not image_data:
        return json.dumps({"error": "Failed to decode image data"}), 400, {'Content-Type': 'application/json'}

    try:
        image_bytes = base64.b64decode(image_data)
        answer = vqa(image_bytes)

        return json.dumps({"answer": answer}), 200, {'Content-Type': 'application/json'}

    except Exception as e:
        return json.dumps({"error": str(e)}), 500, {'Content-Type': 'application/json'}