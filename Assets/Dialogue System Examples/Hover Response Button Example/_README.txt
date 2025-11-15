How to use this example in your own project:

1. Use a Standard Dialogue UI.

2. Add a Tooltip text component to your dialogue UI. Set its text to an empty string.

3. Add the ActivateOnResponseHover script to your response button(s). Assign the
   Tooltip text.

4. In your dialogue entries, you can:
- Set the Description field to use it as tooltip text.
- Add a custom field named 'Activate On Hover' with the name of a GameObject to 
  activate when hovering over the response. This GameObject's root parent must
  be active.
