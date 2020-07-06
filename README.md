# vc-module-dynamic-associations

# Dynamic rule based product associations

## Functionality overview

This module allows to create product association rules. These rules are dynamic, so they are defined by the set of restrctions rather than specific products. So new product which matches rule conditions would be handled by the rule without its change.

Evaluation mechanism returns associated products based on the most prioritized matched rule for the given products.

## Rules management

Rules are located in the Marketing tab.
![Dynamic associations](docs/media/rule-list.png)  

Here is how the rule editor looks like - all properties could be managed here:
![Dynamic associations](docs/media/rule-wizard.png)  

## Usage

For interactions module provides the API. It consists of CRUD operations, preview, and most important - evaluation endpoint.

Assuming we have the single active rule of "Accessories" group for the "Electronics" store which matches all cellphones of ASUS brand. It returns all Binuaral headphones, with the output limit of 10 items.

The following request 
```
POST /api/dynamicassociations/evaluate
```
with such body (`f9330eb5ed78427abb4dc4089bc37d9f` - ASUS 64GB cellphone id in the sample data):
```
{
  "storeId": "Electronics",
  "productsToMatch": [
    "f9330eb5ed78427abb4dc4089bc37d9f"
  ],
  "group": "Accessories",
  "take": 10,
  "skip": 0
}
```
returns:
```
[
  "0f7a77cc1b9a46a29f6a159e5cd49ad1",
  "fb4c38fa746d44ffb70d4f904a7741b0",
  "4a835614487e4d599d17b77652872001",
  "a5bc65b871e947dabb0bc88239c085b3",
  "a9185fbb41d14e1baa355e631f06e8fd",
  "2b6fe6c6779f4cbeaa95c67e0cc7afd6",
  "143e3eb3d1ee4a2bbf8fa0ecacfd1222",
  "6e7a31c35c814fb389dc2574aa142b63",
  "f7624d8ca3334998a46b1dac7d21c990",
  "7252d30d493d4925adb9b586d09c6e09"
]
```