Title: Interstellar

Personal information:
Name: Jason Tai
UtorID: taijason
student number: 1008122363
Assigment Number: A6

Instructions:
1) create a folder called build and cd into it
2) run cmake ..
3) run make
4) run ./Interstellar

Description:
This is a black hole scene inspired by the movie Interstellar.
There is a skydome created in sky_dome.tes, The scene is rendered from inside the dome.
The main effect is implemented in blackhole_lens.fs, which shades the sky dome using either a grid or an imported galaxy texture (toggleable with G).
To create the lensing effect, view directions are converted to spherical coordinates and warped to look like how light would bend around a black hole. These warped directions are then used to sample the background.
There is an animated ring to simulate the look of a photon ring around a black hole.
There are pulsing stars added on top of the galaxy texture
There is a subtle turquoise gradient that pulses that is added on top of the galaxy texture.
The grid background has a gradient background tone.
Music was added in main.cpp
A slow rotation is added in main.cpp
Toggles for switching the background and music are implemented in main.cpp

List of features:
sky dome
lens effect
texture usage
music
animation
toggles

Acknowledgements:

Texture stuff:
galaxy texture: https://www.spacespheremaps.com/multi-nebulae-spheremaps/
stb_image.h library: https://github.com/nothings/stb/blob/master/stb_image.h

Music stuff:
miniaudio.h Library: https://github.com/mackron/miniaudio/blob/master/miniaudio.h
music: Interstellar Main Theme - Hans Zimmer
https://www.youtube.com/watch?v=kpz8lpoLvrA
