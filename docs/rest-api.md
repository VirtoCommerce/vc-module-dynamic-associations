# Rest API

!!! attention
    This is an auto-generated document. Used [swagger-markdown](https://github.com/syroegkin/swagger-markdown).

## Endpoints

### /api/dynamicassociations/search

#### POST
##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |

### /api/dynamicassociations/{id}

#### GET
##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| id | path |  | Yes | string |

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |

### /api/dynamicassociations/new

#### GET
##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | catalog:create |

### /api/dynamicassociations

#### POST
##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |

#### DELETE
##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| ids | query |  | No | [ string ] |

##### Responses

| Code | Description |
| ---- | ----------- |
| 204 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |

### /api/dynamicassociations/evaluate

#### POST
##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |

### /api/dynamicassociations/preview

#### POST
##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 401 | Unauthorized |
| 403 | Forbidden |

##### Security

| Security Schema | Scopes |
| --- | --- |
| oauth2 | |
