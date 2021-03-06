var matchId,
    leadTracker = true,
    intervalHandle,
    leadT1 = 1,
    leadT2 = 0;

function onRadioChange()
{
    //checked which one is now checked
    leadTracker = document.getElementById('radio-place-first-t1').checked;

    leadT1 = leadTracker ? 1 : 0;
    leadT2 = leadTracker ? 0 : 1;

    if ( leadTracker )
    {
	$( '#' + 't1-' + 'place-first' ).addClass( 'lead' );
	$( '#' + 't1-' + 'place-second' ).removeClass( 'lead' );

	$( '#' + 't2-' + 'place-first' ).removeClass( 'lead' );
	$( '#' + 't2-' + 'place-second' ).addClass( 'lead' );
    }
    else
    {
	$( '#' + 't2-' + 'place-first' ).addClass( 'lead' );
	$( '#' + 't2-' + 'place-second' ).removeClass( 'lead' );

	$( '#' + 't1-' + 'place-first' ).removeClass( 'lead' );
	$( '#' + 't1-' + 'place-second' ).addClass( 'lead' );
    }

    $('input[type="radio"][value="t1"]').prop('checked', leadTracker);
    $('input[type="radio"][value="t2"]').prop('checked', !leadTracker);

    $.post( 'server.php', {
	match: matchId,
	state: {
	    playerLead:
	    {
		p1: leadT1,
		p2: leadT2
	    }
	}
    }, function ( res ) {
	$( '#sync-message' )
	    .text( 'Leader sent successfully.' )
	    .fadeIn( 50 )
	    .delay( 3000 )
	    .fadeOut( 1000 );
    } );
}

function onCheckboxChange() {
    // id is of format `<t1|t2>-<ID of corresponding image>`
    var id = this.id,
	imgId = id.substr( 6 ),
	state = this.checked,
	data = {
	    match: matchId,
	    state: {
		playerLead:
		{
		    p1: leadT1,
		    p2: leadT2
		}
	    }
	}
    //var curLeadTracker = $('input[name="place-first"]:checked').val();
    //leadTracker = curLeadTracker;

    data.state[imgId] = state;
    $( '#' + imgId ).toggleClass( 'show' );
    $( '#' + imgId + '-g' ).toggleClass( 'show' );

    $.post( 'server.php', data, function ( res ) {
	$( '#sync-message' )
	    .text( 'Data sent successfully.' )
	    .fadeIn( 50 )
	    .delay( 3000 )
	    .fadeOut( 1000 );
    } );
}

function updateState( state ) {
    leadT1 = state.playerLead.p1;
    leadT2 = state.playerLead.p2;

    if ( leadT1 == 1 )
    {
	leadTracker = true;
	if ( leadT2 == 1 )
	{
	    leadT2 = 0;
	}
    }
    else
    {
	if ( leadT2 == 1 )
	{
	    leadTracker = false;
	}
	else
	{
	    leadT1 = 1;
	    leadT2 = 0;
	    leadTracker = true;
	}
    }
    
    if ( leadTracker )
    {
	$( '#' + 't1-' + 'place-first' ).addClass( 'lead' );
	$( '#' + 't1-' + 'place-second' ).removeClass( 'lead' );

	$( '#' + 't2-' + 'place-first' ).removeClass( 'lead' );
	$( '#' + 't2-' + 'place-second' ).addClass( 'lead' );

    }
    else
    {
	$( '#' + 't2-' + 'place-first' ).addClass( 'lead' );
	$( '#' + 't2-' + 'place-second' ).removeClass( 'lead' );

	$( '#' + 't1-' + 'place-first' ).removeClass( 'lead' );
	$( '#' + 't1-' + 'place-second' ).addClass( 'lead' );
	
    }

    $('input[type="radio"][value="t1"]').prop('checked', leadTracker);
    $('input[type="radio"][value="t2"]').prop('checked', !leadTracker);

    delete state.playerLead;

    var ids = Object.keys( state );

    // We want to find everything that's currently shown EXCEPT
    // those images which are shown *by default* since we're going
    // to check these results against the provided state and hide
    // any that don't match the server.
    $( '.show' ).not( '[id$="-g"]' ).each( function ( _, $el ) {
	if ( ids.indexOf( $el.id ) < 0 ) {
	    $( this ).removeClass( 'show' );
	    $( '#' + $el.id + '-g' ).addClass( 'show' );
	    $( '#check-' + $el.id ).prop( 'checked', false );
	}
    } );

    // Light up everything sent by the server, even if we just turned it off
    ids.forEach( function ( id ) {
	$( '#' + id ).addClass( 'show' );
	$( '#' + id + '-g' ).removeClass( 'show' );
	$( '#check-' + id ).prop( 'checked', true );
    } );
}

function linkMatch( e ) {
    if ( intervalHandle ) {
	clearInterval( intervalHandle );
	intervalHandle = undefined;
	$( 'form input[type="submit"]' ).val( 'Link' );
	return false;
    }

    matchId = $( '#match' ).val();
    
    $.get( 'server.php', { match: matchId }, function ( res ) {
	if ( Array.isArray( res ) && !res.length ) {
	    $( '#sync-message' )
		.text( 'Match not found. New match created.' )
		.css( 'color', 'black' )
		.fadeIn( 50 );
	    //this should also "link" it now
	    return;
	}
	updateState( res );
	intervalHandle = setInterval( function () {
	    $.get( 'server.php', { match: matchId }, function ( res ) {
		updateState( res );
	    } );
	}, 250 );

	$( 'form input[type="submit"]' ).val( 'End link' );
	$( '#sync-message' )
	    .text( 'Link established successfully. Now updating every 250ms.' )
	    .fadeIn( 50 )
	    .delay( 3000 )
	    .fadeOut( 1000 );
    } );

    e.preventDefault();
}

$( 'body' ).on( 'change', 'input[type="checkbox"]', onCheckboxChange );
$( '#matchForm' ).on( 'submit', linkMatch );
$( 'body' ).on( 'change', 'input[type="radio"]', onRadioChange );
//$( 'body' ).on( 'clicked', 'input[type="radio"]', onRadioChange );
