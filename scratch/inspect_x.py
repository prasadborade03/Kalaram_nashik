import urllib.request
import json

def post(url, payload, headers):
    req = urllib.request.Request(url, headers=headers, data=json.dumps(payload).encode('utf-8'))
    with urllib.request.urlopen(req) as resp:
        return resp.headers, resp.read().decode('utf-8')

headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
h, init_res = post('http://127.0.0.1:8080/mcp', {
    'jsonrpc': '2.0', 'id': 1, 'method': 'initialize',
    'params': {'protocolVersion': '2024-11-05', 'capabilities': {}, 'clientInfo': {'name': 'test-client', 'version': '1.0'}}
}, headers)

session_id = h.get('mcp-session-id')
headers['mcp-session-id'] = session_id

post('http://127.0.0.1:8080/mcp', {
    'jsonrpc': '2.0', 'method': 'notifications/initialized'
}, headers)

csharp_code = """
var kalaramObj = GameObject.Find("kalaram_glb");
var mf = kalaramObj.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var vertices = mesh.vertices;

// Check width profile along X
float minX = 999f, maxX = -999f;
foreach(var v in vertices) {
    Vector3 worldV = kalaramObj.transform.TransformPoint(v);
    if (worldV.x < minX) minX = worldV.x;
    if (worldV.x > maxX) maxX = worldV.x;
}
return $"MinX: {minX:F2}, MaxX: {maxX:F2}";
"""

_, exec_res = post('http://127.0.0.1:8080/mcp', {
    'jsonrpc': '2.0', 'id': 6, 'method': 'tools/call',
    'params': {
        'name': 'execute_code',
        'arguments': {
            'action': 'execute',
            'code': csharp_code
        }
    }
}, headers)

res_json = json.loads(exec_res.split('data: ')[1])
print(res_json['result']['content'][0]['text'])
