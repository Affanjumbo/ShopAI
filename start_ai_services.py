import subprocess

services = [
    "AI-Models.RecommendationSystem.scripts.model_serving:app",
    "AI-Models.ExpenseManager.scripts.expense_manager:app",
    "AI-Models.SmartSuggestionAI.main:app"
]

ports = [8000, 8001, 8002]

for i, service in enumerate(services):
    subprocess.Popen([
        "python", "-m", "uvicorn", service,
        "--host", "127.0.0.1",
        "--port", str(ports[i]),
        "--reload"
    ])
