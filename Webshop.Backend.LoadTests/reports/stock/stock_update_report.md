> test info

test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2025-05-06_08.55.44_session_2d44b33d`

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
|latency|min = `0,92`, mean = `2,66`, max = `16,7`, StdDev = `0,99`|
|latency percentile|p50 = `2,42`, p75 = `3,08`, p95 = `4,19`, p99 = `5,14`|
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
|latency|min = `1,04`, mean = `2,66`, max = `16,75`, StdDev = `0,94`|
|latency percentile|p50 = `2,44`, p75 = `3,07`, p95 = `4,16`, p99 = `5,09`|
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
|latency|min = `4,11`, mean = `8,35`, max = `25,34`, StdDev = `2,78`|
|latency percentile|p50 = `7,56`, p75 = `9,51`, p95 = `13,98`, p99 = `18,8`|
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
|latency|min = `2,38`, mean = `5,99`, max = `20,34`, StdDev = `2,24`|
|latency percentile|p50 = `5,36`, p75 = `6,85`, p95 = `10,47`, p99 = `13,73`|
|data transfer|min = `0,06` KB, mean = `0,065` KB, max = `0,069` KB, all = `0,4` MB|


> status codes for scenario: `ws_stock_update`

|status code|count|message|
|---|---|---|
|no status|6000||


