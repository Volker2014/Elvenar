<?php
include ('render_elvenar_data.php');
?>
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en" dir="ltr">
<head>
	<title>Liste der Elvenar Gebäude</title>
	<meta http-equiv="Content-Type" lang="en" content="text/html; charset=utf-8" />
	<meta name="language" content="en"/>
	<meta name="copyright" content="GNU General Public License, http://www.gnu.org/licenses/gpl.html" />
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link rel="stylesheet" type="text/css" href="elvenar_city.css" title="default" media="screen" />
</head>
	<body>
		<div>
			<h2>Elvenar Gebäude</h2>
			<?php
				if(function_exists('render_elvenar_data')){
					render_elvenar_data('elvenar_city.elc');
				}else{
					echo null;//allows the page to continue rendering
				}
			?>
		</div>
	</body>
</html>