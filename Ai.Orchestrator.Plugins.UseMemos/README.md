# UseMemos Plugin

## UseMemos API documentation
- https://memos.apidocumentation.com/reference

## Example Config
```
{
	"description": "A plugin for interfacing with a UseMemos server",
	"contract": {
		"method": "read,add,edit",
		"getType": "memos,resources?",
		"uid": "string?"
	},
	"memoAccount": {
		"apiKey": "__API_KEY__",
		"memosUrl": "http://address"
	}
}
```