# CoinAsBattery_NWPluginAPI
Plugin CoinAsBattery for NW PluginAPI. Enables using Coins as battery for Radio and MicroHID.
- An item is charged, when player picks Coin and has equipped Radio and/or MicroHID.
- Upon picking, battery capacity is being added to current battery/energy level.
- If player has both items, Radio will be charged first.
- Radio must be turned ON or have battery discharged in order to be charged.
- MicroHID has option to explode, if charged when energy was full. Use "pick" to set explosion on picking Coin. Use "fire" to set explosion, when overcharged MircoHID is in firing state. Use anything else to disable this feature.

## Config
|Name|Type|Default value|Description|
|---|---|---|---|
|is_enabled|bool|true|Is plugin enabled?|
|debug|bool|false|Should Debug be enabled?|
|radio_battery_capacity|byte|30|Radio battery capacity. Set between 0-100.|
|micro_battery_capacity|float|0.2|MicroHID battery capacity. Set between 0-1.|
|should_explode|string|nope|Should MicroHID explode, if charged when full? Type "pick" to set explosion after picking coin, type "fire" to set explosion when firing overcharged micro.|
