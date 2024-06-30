# inertial_scroll_sample_code

I am working on a Jigsaw Themed game in which the jigsaw-pieces are arranged in 'drawers' located near the left and right edges of the screen.

I have implemented inertial scroll in this code. 

The class, "Drawer" is a Monobehaviour that I attach with a simple sprite game-object with a Collider component also attached to it as a 'trigger'. 

An instance of "DrawerScroll" create inside "Drawer" looks after the scrolling of the jigsaw-pieces in the drawer.

The class, "JigsawPiece" represents a piece of the jigsaw-puzzle.

"JigsawSet" represents a complete set of all pieces of a jigsaw-puzzle.

This is a small part of a bigger system that I am working on. There are still some "ToDo" points in the code but this code should give you a fair idea about my work in Unity.
