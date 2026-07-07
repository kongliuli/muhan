import json
import sys


def query(params):
    term = (params.get("search") or params.get("rawQuery") or "").strip()
    if not term:
        return []
    return [
        {
            "Title": f"Python: {term}",
            "SubTitle": "JsonRpc 示例插件",
            "IcoPath": "",
            "Score": 60,
            "ActionCommand": f'echo hello-{term}',
        }
    ]


def main():
    line = sys.stdin.readline()
    if not line:
        return
    req = json.loads(line)
    method = req.get("method")
    req_id = req.get("id")
    params = req.get("params") or {}

    if method == "query":
        result = query(params)
    else:
        print(json.dumps({"jsonrpc": "2.0", "id": req_id, "error": {"message": f"unknown method: {method}"}}))
        return

    print(json.dumps({"jsonrpc": "2.0", "id": req_id, "result": result}))


if __name__ == "__main__":
    main()
