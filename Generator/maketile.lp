%Starting dimensions
xcoord(0..3).
ycoord(0..3).

%Starting list of actions
action(walk).

%a location is an xcoord and a ycoord
loc(X,Y):- xcoord(X),ycoord(Y).

%Choose 4 places to tile.
4{tile(loc(X,Y)) : loc(X,Y)}8.

%Choose 1 start loc.
1{start(loc(X,Y)) : loc(X,Y)}1.

%Can't start IN a tile.
:- start(loc(X,Y)),tile(loc(X,Y)).

%I need to land on a tile to start
land :- start(loc(X,Y)), tile(loc(X,Z)), Z > Y.
:-not land.

%Choose 1 end loc.
1{end(loc(X,Z)) : loc(X,Z)}1.

%End can't be start, and can't be IN tile.
:- start(loc(X,Y)),end(loc(X,Y)).
:- end(loc(X,Y)), tile(loc(X,Y)).

%End location MUST start on ground.
top :- end(loc(X,Y)),tile(loc(X,Z)), Z=Y-1.
:- not top.

%an open location is one without a tile on it.
open(loc(X,Y)):-loc(X,Y),not tile(loc(X,Y)).

%horizontal walk reachable
%reachable(loc(X,Y),loc(Z,Y)):- open(X,Y),open(Z,Y),
%horizontal walk symmetry
%reachable(loc(X,Y),loc(Z,Y), action(walk)):-reachable(loc(Z,Y),loc(X,Y),action(walk)).


%Vert Jump Reachable stuff here


%path(loc(X1,Y1),loc(X2,Y2),S):-

%completeable:- path(loc(X1,Y1),loc(X2,Y2),S),start(X1,Y1),end(X2,Y2).