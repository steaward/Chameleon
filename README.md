### Chameleon

This project is a WPF application which attaches itself to a running process.
It is fully transparant and the running process's main window handle is what is in view.

It will allow the user to record their actions (mouse/keyboard events) within this running process.
After recording the actions, you can play back the records in real time. 

It records each user action (and inaction) as a 'Frame' then plays back all frames sequentially.

Hooks directly into pinvoke functionality. Playback does not require control of mouse or keyboard!


#### Major list of to-dos:
- Started: OpenCV image processing  
       - Allow recordings to look for specific objects  
       - Draw boxes over the found objects in the windows form.  
       - Automate response to objects (click on found objects)  

