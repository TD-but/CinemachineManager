
# CineMachine Manager

Custom unity tool that allows easy setting up and transitioning of multiple Cinemachine virtual cameras.


## Features

- Easily create and setup multiple virtual cameras
- Transition between cameras without FSM triggers 
- Create prefab linked virtual cameras in scene allowing mass changes to cameras and properties



## Installation

You can add this library to your project using the Package Manager.

Go to the package manager and click on "Add package from git URL".
From there, add this URL:

```
https://github.com/TD-but/CinemachineManager.git
```
    
## API Reference

#### Switch To Camera

```c#
  CineCameraManager.Instance.SwitchTo(id);
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `id` | `integer` | **Required**. Unique ID of camera to transition to |



## Configuration
1. Add Cinemachine Brain Component to MainCamera in scene.

    ![Main Camera Screenshot with CineBrain component](https://i.postimg.cc/3wrtXV8v/Main-Camera-Screenshot.png)

2. Add Cinemachine Manager script or get prefab from package/CinemachineManager/Prefabs folder
3. If you have added new script component to a gameobject then set the virtual camera prefab reference

    ![Setting Virtual Camera Prefab Screenshot](https://i.postimg.cc/G3vS1k59/Setting-Virtual-Camera-Screenshot.png)
