<?php
 header("Access-Control-Allow-Origin: *");
 
function getMatch( $id ) {
	$filename = $id . '.json';

	if ( file_exists( $filename ) ) {
		return json_decode( file_get_contents( $filename ), true );
	}
	return array();
}

function setMatch( $id, $data ) {
	$filename = $id . '.json';
	file_put_contents( $filename, json_encode( $data ) );
}

header( 'Content-type: application/json' );

if ( $_SERVER['REQUEST_METHOD'] === 'POST' ) {
	if ( !isset( $_POST['match'] ) ) {
		// Reject the request
		http_response_code( 400 );
		die();
	}

	$matchId = $_POST['match'];

	$state = getMatch( $matchId );

	$state['mapstones'] = $_POST['state']['mapstones'];
	unset( $_POST['state']['mapstones'] );

	$key = array_keys( $_POST['state'] )[0];

	if ( array_key_exists( $key, $state ) && $_POST['state'][$key] === 'false' ) {
		unset( $state[$key]);
	} elseif ( !array_key_exists( $key, $state ) && $_POST['state'][$key] ) {
		$state[$key] = TRUE;
	}

	setMatch( $matchId, $state );

	echo json_encode( $state );
} else {
       // file_put_contents( 'asdf.txt', 'test' );
       // $contents = file_get_contents( 'asdf.txt' );
       // die( $contents );
       if ( !isset( $_GET['match'] ) ) {
		// Reject the request
		http_response_code( 400 );
		die();
	}

	$matchId = $_GET['match'];
	$state = getMatch( $matchId );

	echo json_encode( $state );
}
?>
