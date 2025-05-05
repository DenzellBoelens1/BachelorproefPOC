> test info

test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2025-05-02_11.27.56_session_e08fb013`

> scenario stats

scenario: `rest_stock_update`

  - ok count: `6000`

  - fail count: `0`

  - all data: `0,5` MB

  - duration: `00:01:00`

load simulations:

  - `inject`, rate: `100`, interval: `00:00:01`, during: `00:01:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `6000`, ok = `6000`, RPS = `100`|
|latency|min = `2,38`, mean = `3,91`, max = `18,21`, StdDev = `0,74`|
|latency percentile|p50 = `3,76`, p75 = `4,12`, p95 = `4,92`, p99 = `6,52`|
|data transfer|min = `0,071` KB, mean = `0,077` KB, max = `0,081` KB, all = `0,5` MB|


> status codes for scenario: `rest_stock_update`

|status code|count|message|
|---|---|---|
|no status|6000||


> scenario stats

scenario: `graphql_stock_update`

  - ok count: `6000`

  - fail count: `0`

  - all data: `2,3` MB

  - duration: `00:01:00`

load simulations:

  - `inject`, rate: `100`, interval: `00:00:01`, during: `00:01:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `6000`, ok = `6000`, RPS = `100`|
|latency|min = `2,27`, mean = `3,95`, max = `19,62`, StdDev = `0,87`|
|latency percentile|p50 = `3,79`, p75 = `4,17`, p95 = `4,99`, p99 = `7,04`|
|data transfer|min = `0,383` KB, mean = `0,388` KB, max = `0,391` KB, all = `2,3` MB|


> status codes for scenario: `graphql_stock_update`

|status code|count|message|
|---|---|---|
|no status|6000||


> scenario stats

scenario: `signalr_stock_update`

  - ok count: `6000`

  - fail count: `0`

  - all data: `0,4` MB

  - duration: `00:01:00`

load simulations:

  - `inject`, rate: `100`, interval: `00:00:01`, during: `00:01:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `6000`, ok = `6000`, RPS = `100`|
|latency|min = `7,39`, mean = `11,66`, max = `25,55`, StdDev = `1,76`|
|latency percentile|p50 = `11,46`, p75 = `12,46`, p95 = `14,74`, p99 = `18,14`|
|data transfer|min = `0,062` KB, mean = `0,068` KB, max = `0,072` KB, all = `0,4` MB|


> status codes for scenario: `signalr_stock_update`

|status code|count|message|
|---|---|---|
|no status|6000||


> scenario stats

scenario: `ws_stock_update`

  - ok count: `6000`

  - fail count: `0`

  - all data: `0,4` MB

  - duration: `00:01:00`

load simulations:

  - `inject`, rate: `100`, interval: `00:00:01`, during: `00:01:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `6000`, ok = `6000`, RPS = `100`|
|latency|min = `5,05`, mean = `8,33`, max = `22,7`, StdDev = `1,57`|
|latency percentile|p50 = `8,04`, p75 = `9,06`, p95 = `11,22`, p99 = `13,18`|
|data transfer|min = `0,06` KB, mean = `0,065` KB, max = `0,069` KB, all = `0,4` MB|


> status codes for scenario: `ws_stock_update`

|status code|count|message|
|---|---|---|
|no status|6000||


