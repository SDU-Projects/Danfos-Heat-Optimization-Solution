import json
import os
from typing import Any

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from openai import OpenAI
from pydantic import BaseModel

app = FastAPI(title="Heat-Optimization AI Assistant", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

client = OpenAI(api_key=os.environ.get("OPENAI_API_KEY", ""))

# ── Pydantic models ────────────────────────────────────────────────────────────

class ChatTurn(BaseModel):
    role: str   # "user" | "assistant"
    content: str

class ChatRequest(BaseModel):
    messages: list[ChatTurn]

class UnitData(BaseModel):
    name: str
    type: str                       # GasBoiler | OilBoiler | GasMotor | ElectricBoiler
    max_heat_mw: float
    production_cost_per_mwh: float
    co2_kg_per_mwh: float
    electricity_produced_mw: float  # positive = produces, 0 for heat-only
    electricity_consumed_mw: float  # positive = consumes (ElectricBoiler), 0 otherwise
    image_url: str

class ChatResponse(BaseModel):
    reply: str
    unit_data: UnitData | None = None

# ── System prompt ──────────────────────────────────────────────────────────────

SYSTEM_PROMPT = """
You are an engineering assistant for a district-heating optimization platform.
Your job is to help the operator create a new Production Unit through a friendly
step-by-step conversation.

Available production unit types and their characteristics:
  • GasBoiler      – heat only, uses gas, no electricity interaction
  • OilBoiler      – heat only, uses oil, no electricity interaction
  • GasMotor       – CHP (Combined Heat & Power), produces BOTH heat AND electricity
                     (electricity_produced_mw > 0, electricity_consumed_mw = 0)
  • ElectricBoiler – consumes electricity to produce heat
                     (electricity_consumed_mw > 0, electricity_produced_mw = 0)

Fields you must collect (ask one or two at a time, never dump all questions at once):
  1. name                       – short identifier, e.g. "GB4"
  2. type                       – one of the four types above
  3. max_heat_mw                – maximum thermal output in MW (positive number)
  4. production_cost_per_mwh    – fuel/operating cost in DKK/MWh of heat produced
  5. co2_kg_per_mwh             – CO₂ emissions in kg per MWh of heat produced
  6. electricity_produced_mw    – only for GasMotor; MW of electricity generated when running at full capacity (else 0)
  7. electricity_consumed_mw    – only for ElectricBoiler; MW of electricity consumed at full capacity (else 0)
  8. image_url                  – a short filename like "gb4.png" (suggest sensible defaults, e.g. "gasboiler.png")

Conversation rules:
  - Be concise and professional.
  - Ask for confirmation before finalising.
  - Once you have all data and the user confirms, respond with a plain-text summary AND
    embed a single JSON object in your reply using exactly this marker format:

    UNIT_JSON_START
    { ...json... }
    UNIT_JSON_END

    The JSON must have exactly these keys (use 0 for irrelevant electricity fields):
    {
      "name": "...",
      "type": "...",
      "max_heat_mw": 0.0,
      "production_cost_per_mwh": 0.0,
      "co2_kg_per_mwh": 0.0,
      "electricity_produced_mw": 0.0,
      "electricity_consumed_mw": 0.0,
      "image_url": "..."
    }

  - Never emit the JSON block until the user has confirmed all details.
  - If the user asks a general question about the system, answer it helpfully, but
    steer the conversation back toward creating a unit.
"""


def _extract_unit_json(text: str) -> UnitData | None:
    """Parse UNIT_JSON_START ... UNIT_JSON_END block from the assistant reply."""
    start_marker = "UNIT_JSON_START"
    end_marker = "UNIT_JSON_END"
    start = text.find(start_marker)
    end = text.find(end_marker)
    if start == -1 or end == -1:
        return None
    raw = text[start + len(start_marker):end].strip()
    try:
        data: dict[str, Any] = json.loads(raw)
        return UnitData(**data)
    except Exception:
        return None


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/chat", response_model=ChatResponse)
def chat(request: ChatRequest) -> ChatResponse:
    if not client.api_key:
        raise HTTPException(
            status_code=503,
            detail="OPENAI_API_KEY environment variable is not set.",
        )

    messages = [{"role": "system", "content": SYSTEM_PROMPT}]
    for turn in request.messages:
        messages.append({"role": turn.role, "content": turn.content})

    try:
        completion = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            temperature=0.4,
            max_tokens=600,
        )
    except Exception as exc:
        raise HTTPException(status_code=502, detail=f"OpenAI error: {exc}") from exc

    reply_text: str = completion.choices[0].message.content or ""
    unit_data = _extract_unit_json(reply_text)

    clean_reply = reply_text
    start = clean_reply.find("UNIT_JSON_START")
    if start != -1:
        end = clean_reply.find("UNIT_JSON_END")
        if end != -1:
            clean_reply = (clean_reply[:start] + clean_reply[end + len("UNIT_JSON_END"):]).strip()

    return ChatResponse(reply=clean_reply, unit_data=unit_data)
