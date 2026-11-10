<?php
/**
 * Plugin Name: TrackerSQL User Roles (Woo API)
 * Description: Lets TrackerSQL grant/remove WordPress roles using existing WooCommerce REST API keys (no Application Password / Authorization header).
 * Version: 1.0.0
 *
 * Install: copy to wp-content/mu-plugins/tracker-user-roles.php on the shop (must-use folder, not regular plugins).
 */

if ( ! defined( 'ABSPATH' ) ) {
	exit;
}

/**
 * Let WooCommerce authenticate consumer key/secret on wc-tracker/* routes (query string or Basic).
 */
add_filter(
	'woocommerce_rest_is_request_to_rest_api',
	function ( $is_wc_request ) {
		if ( $is_wc_request || empty( $_SERVER['REQUEST_URI'] ) ) {
			return $is_wc_request;
		}
		$uri = esc_url_raw( wp_unslash( $_SERVER['REQUEST_URI'] ) );
		return ( false !== strpos( $uri, '/wc-tracker/' ) );
	}
);

add_action(
	'rest_api_init',
	function () {
		register_rest_route(
			'wc-tracker/v1',
			'/ping',
			array(
				'methods'             => WP_REST_Server::READABLE,
				'callback'            => 'tracker_sql_rest_ping',
				'permission_callback' => 'tracker_sql_rest_can_edit_users',
			)
		);

		register_rest_route(
			'wc-tracker/v1',
			'/users/roles',
			array(
				'methods'             => WP_REST_Server::EDITABLE,
				'callback'            => 'tracker_sql_rest_update_user_roles',
				'permission_callback' => 'tracker_sql_rest_can_edit_users',
				'args'                => array(
					'email'       => array(
						'required'          => true,
						'type'              => 'string',
						'sanitize_callback' => 'sanitize_email',
					),
					'add_role'    => array(
						'type'              => 'string',
						'sanitize_callback' => 'sanitize_key',
					),
					'remove_role' => array(
						'type'              => 'string',
						'sanitize_callback' => 'sanitize_key',
					),
				),
			)
		);
	}
);

/**
 * @return bool|WP_Error
 */
function tracker_sql_rest_can_edit_users() {
	$user = wp_get_current_user();
	if ( ! $user || ! $user->exists() ) {
		return new WP_Error(
			'tracker_rest_not_logged_in',
			'WooCommerce API key did not authenticate. Use Read/Write keys from an administrator.',
			array( 'status' => 401 )
		);
	}
	if ( ! user_can( $user, 'edit_users' ) ) {
		return new WP_Error(
			'tracker_rest_forbidden',
			'The API key user cannot edit other users.',
			array( 'status' => 403 )
		);
	}
	return true;
}

/**
 * @return WP_REST_Response
 */
function tracker_sql_rest_ping() {
	$user = wp_get_current_user();
	return rest_ensure_response(
		array(
			'ok'      => true,
			'user_id' => (int) $user->ID,
			'login'   => $user->user_login,
			'roles'   => array_values( (array) $user->roles ),
		)
	);
}

/**
 * @param WP_REST_Request $request
 * @return WP_REST_Response|WP_Error
 */
function tracker_sql_rest_update_user_roles( WP_REST_Request $request ) {
	$email  = $request->get_param( 'email' );
	$add    = $request->get_param( 'add_role' );
	$remove = $request->get_param( 'remove_role' );

	if ( empty( $add ) && empty( $remove ) ) {
		return new WP_Error( 'tracker_no_role', 'Specify add_role and/or remove_role.', array( 'status' => 400 ) );
	}

	$user = get_user_by( 'email', $email );
	if ( ! $user ) {
		return new WP_Error( 'tracker_user_not_found', 'No WordPress user with that email.', array( 'status' => 404 ) );
	}

	$wp_user = new WP_User( $user->ID );

	if ( ! empty( $remove ) ) {
		$wp_user->remove_role( $remove );
	}
	if ( ! empty( $add ) ) {
		$wp_user->add_role( $add );
	}
	if ( ! in_array( 'customer', (array) $wp_user->roles, true ) ) {
		$wp_user->add_role( 'customer' );
	}

	return rest_ensure_response(
		array(
			'user_id' => (int) $wp_user->ID,
			'email'   => $wp_user->user_email,
			'roles'   => array_values( (array) $wp_user->roles ),
		)
	);
}
