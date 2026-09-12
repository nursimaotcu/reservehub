"""Run against a disposable, running PostgreSQL-backed API. Python 3, no packages."""
import concurrent.futures
import datetime as dt
import json
import os
import urllib.error
import urllib.request
import uuid

BASE = os.environ.get('API_URL', 'http://127.0.0.1:5080').rstrip('/')

def request(method, path, body=None, token=None):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    req = urllib.request.Request(BASE + path, data=json.dumps(body).encode() if body is not None else None, headers=headers, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=20)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        raw = response.read()
        return response.status, json.loads(raw) if raw else None

def check(status, expected):
    assert status == expected, f'Expected HTTP {expected}, got {status}'

def main():
    check(request('GET', '/health/ready')[0], 200)
    password = 'Demo-password-' + uuid.uuid4().hex
    users = []
    for _ in range(2):
        email = uuid.uuid4().hex + '@example.test'
        check(request('POST', '/api/auth/register', {'email': email, 'password': password})[0], 201)
        code, login = request('POST', '/api/auth/login', {'email': email, 'password': password})
        check(code, 200)
        users.append(login['accessToken'])
    check(request('POST', '/api/auth/register', {'email': email.upper(), 'password': password})[0], 409)
    check(request('POST', '/api/auth/login', {'email': email, 'password': 'wrong-password-value'})[0], 401)
    check(request('POST', '/api/auth/register', {'email': 'invalid', 'password': 'short'})[0], 400)
    check(request('GET', '/api/bookings/')[0], 401)
    check(request('GET', '/api/bookings/', token='invalid-token')[0], 401)
    check(request('POST', '/api/rooms/', {'name': 'Forbidden', 'capacity': 4}, users[0])[0], 403)
    code, login = request('POST', '/api/auth/login', {'email': os.environ['ADMIN_EMAIL'], 'password': os.environ['ADMIN_PASSWORD']})
    check(code, 200)
    admin = login['accessToken']
    check(request('POST', '/api/rooms/', {'name': 'Invalid', 'capacity': 0}, admin)[0], 400)
    code, room = request('POST', '/api/rooms/', {'name': 'Test ' + uuid.uuid4().hex, 'capacity': 6}, admin)
    check(code, 201)
    start = dt.datetime.now(dt.timezone.utc) + dt.timedelta(days=2)
    end = start + dt.timedelta(hours=1)
    booking = {'roomId': room['id'], 'start': start.isoformat(), 'end': end.isoformat()}
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        results = list(pool.map(lambda token: request('POST', '/api/bookings/', booking, token), users))
    assert sorted(code for code, _ in results) == [201, 409], results
    owner = next(i for i, result in enumerate(results) if result[0] == 201)
    bid = results[owner][1]['id']
    path = '/api/bookings/' + bid
    check(request('GET', path, token=users[1-owner])[0], 404)
    check(request('DELETE', path, token=users[1-owner])[0], 404)
    check(request('GET', path, token=users[owner])[0], 200)
    code, mine = request('GET', '/api/bookings/', token=users[1-owner])
    check(code, 200)
    assert all(x['id'] != bid for x in mine)
    adjacent = dict(booking, start=end.isoformat(), end=(end + dt.timedelta(hours=1)).isoformat())
    check(request('POST', '/api/bookings/', adjacent, users[owner])[0], 201)
    check(request('POST', '/api/bookings/', dict(booking, end=start.isoformat()), users[owner])[0], 400)
    check(request('POST', '/api/bookings/', dict(booking, roomId=str(uuid.uuid4())), users[owner])[0], 404)
    check(request('GET', '/api/bookings/?page=0', token=users[owner])[0], 400)
    check(request('DELETE', path, token=users[owner])[0], 204)
    check(request('DELETE', path, token=users[owner])[0], 204)
    check(request('POST', '/api/bookings/', booking, users[1-owner])[0], 201)
    check(request('GET', '/openapi/v1.json')[0], 200)
    print('PASS: auth, roles, validation, ownership, concurrent conflict, adjacent slots, cancellation, OpenAPI')

if __name__ == '__main__':
    main()
