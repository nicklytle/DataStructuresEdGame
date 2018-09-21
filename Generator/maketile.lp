%Starting dimensions
xcoord(0..3).
ycoord(0..3).

%Starting list of actions
%action(walk).

width(1..3).
height(1..3).

winCondition(none).

%a location is an xcoord and a ycoord
%loc(X,Y):- xcoord(X),ycoord(Y).

%dim(X,Y):- width(X),height(Y).

4{block(X,Y,W,H): xcoord(X),ycoord(Y),width(W),height(H)}4.

samepos:- block(X,Y,W,H),Y==2.
:- samepos.

1{start(X,Y): xcoord(X),ycoord(Y)}1.
1{goal(X,Y): xcoord(X),ycoord(Y)}1.
1{helicopter(X,Y): xcoord(X),ycoord(Y)}1.



:-start(X,Y),block(X,Y,W,H).
:-start(X,Y),goal(X,Y).
:-goal(X,Y),block(X,Y,W,H).
:-start(X,Y),helicopter(X,Y).
:-helicopter(X,Y),goal(X,Y).
:-helicopter(X,Y),block(X,Y,W,H).


%Choose 4 places to tile.
%4{tile(loc(X,Y)) : loc(X,Y)}8.




%Choose 4 places to tile.
%4{tile(loc(X,Y)) : loc(X,Y)}8.

%Choose 1 start loc.
%1{start(loc(X,Y)) : loc(X,Y)}1.

%Can't start IN a tile.
%:- start(loc(X,Y)),tile(loc(X,Y)).

%I need to land on a tile to start
%land :- start(loc(X,Y)), tile(loc(X,Z)), Z > Y.
%:-not land.

%Choose 1 end loc.
%1{end(loc(X,Z)) : loc(X,Z)}1.

%End can't be start, and can't be IN tile.
%:- start(loc(X,Y)),end(loc(X,Y)).
%:- end(loc(X,Y)), tile(loc(X,Y)).

%End location MUST start on ground.
%top :- end(loc(X,Y)),tile(loc(X,Z)), Z=Y-1.
%:- not top.

%an open location is one without a tile on it.
%open(loc(X,Y)):-loc(X,Y),not tile(loc(X,Y)).


#show block/4.
#show start/2.
#show goal/2.
#show helicopter/2.
#show winCondition/1.