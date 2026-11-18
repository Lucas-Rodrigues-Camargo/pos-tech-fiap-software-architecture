"""Optional pytest plugin: capture a sanitized replay of actual local HTTP traffic."""
import json
import os
from pathlib import Path
import re
import time
from urllib.parse import urlsplit

import requests

EVENTS = []
START = time.monotonic()
ORIGINAL = requests.sessions.Session.request
SCENARIO = ""


def request(self, method, url, *args, **kwargs):
    response = ORIGINAL(self, method, url, *args, **kwargs)
    parsed = urlsplit(url)
    if parsed.hostname == "localhost" and parsed.port in (8181, 8182, 8183):
        try:
            payload = response.json()
        except ValueError:
            payload = None
        status = payload.get("status") if isinstance(payload, dict) else None
        event = {"scenario": SCENARIO, "second": round(time.monotonic() - START, 2),
                 "service": {8181: "OS", 8182: "Billing", 8183: "Execution"}[parsed.port],
                 "method": method.upper(), "path": re.sub(r"/customers/[^/]+", "/customers/[CPF oculto]", parsed.path),
                 "http": response.status_code, "state": status}
        previous = EVENTS[-1] if EVENTS else {}
        # Polling repeats are omitted; transitions and all mutations are retained.
        if method.upper() != "GET" or any(event.get(k) != previous.get(k) for k in ("scenario", "path", "state", "http")):
            EVENTS.append(event)
    return response


def pytest_configure(config):
    requests.sessions.Session.request = request


def pytest_runtest_setup(item):
    global SCENARIO
    SCENARIO = item.name


def pytest_sessionfinish(session, exitstatus):
    requests.sessions.Session.request = ORIGINAL
    output = Path(os.environ["DEMO_TRACE_FILE"])
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps({"exit_status": int(exitstatus), "events": EVENTS}, indent=2), encoding="utf-8")
