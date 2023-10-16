# JobsAndBoids

A sample project demonstrating Unity's Jobs system for GGJ Plovdiv 2023

## About sample

A simple boid simulation is impelmented with simple game objects and Unity's Jobs system.

Branches:
* `main` - game object implementation
* `jobs` - jobs implementation

<br/><br/><br/><br/><br/>
# Plovdiv Global Game Jam 2023 - Talk Outline

1. Moore's law
    * ![Moorslaw](https://royalsocietypublishing.org/cms/asset/01832a67-6a3b-49c0-9464-51c963351b01/rsta20190061f02.jpg)

1. Multithreading

1. Multithreading issues
    * Racing-condition 
        * ![RaceCondition](https://devopedia.org/images/article/14/3484.1489303713.png)
    * Deadlocks 
        * ![DeadLocks](https://i0.wp.com/tutorialwing.com/wp-content/uploads/2018/10/tutoriawing-os-deadlock-example.png?w=468&ssl=1)

1. Unity's solution - Job's System
    * A thread pool solution

1. Job Types
    * **IJob**: Runs a single task on a job thread.
    * **IJobParallelFor**: Runs a task in parallel. Each worker thread that runs in parallel has an exclusive index to access shared data between worker threads safely.
    * **IJobParallelForTransform**: Runs a task in parallel. Each worker thread running in parallel has an exclusive Transform from the transform hierarchy to operate on.
    * **IJobFor**: The same as IJobParallelFor, but allows you to schedule the job so that it doesn’t run in parallel
    
1. Code demonstration
    * Make a simple job moving objects inside a containment 
    * Job dependencies

1. Burst compiler
    * Blittable data types
        * Have the same memory layout in safe and unsafe code
