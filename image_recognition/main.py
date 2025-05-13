import os
import json
import vertexai
from vertexai.preview.vision_models import Image, ImageTextModel
from flask import Request  # Assuming this is used in a Cloud Function

PROJECT_ID = os.getenv("PROJECT_ID")
LOCATION = os.getenv("REGION", "us-central1")
QUESTION = "What is the object in the image? (choose from apple, banana, orange and grape)"

def vqa(input_file: str) -> str:
    """Run Visual Question Answering on an input image file."""
    vertexai.init(project=PROJECT_ID, location=LOCATION)

    model = ImageTextModel.from_pretrained("imagetext@001")
    source_img = Image.load_from_file(location=input_file)

    answers = model.ask_question(
        image=source_img, 
        question=QUESTION, 
        number_of_results=1
    )

    return answers[0].text.lower() if answers else None

def main(request: Request):
    """HTTP Cloud Function to handle VQA request."""
    try:
        request_json = request.get_json()
        input_file = request_json.get("input_file")

        if not input_file:
            return json.dumps({"error": "Missing 'input_file' parameter"}), 400, {'Content-Type': 'application/json'}

        answer = vqa(input_file)

        return json.dumps({"answer": answer}), 200, {'Content-Type': 'application/json'}

    except Exception as e:
        return json.dumps({"error": str(e)}), 500, {'Content-Type': 'application/json'}