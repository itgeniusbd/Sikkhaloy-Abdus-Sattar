<?php
$uri = urldecode(parse_url($_SERVER["REQUEST_URI"], PHP_URL_PATH) ?? "/");
$root = __DIR__;
$file = $root . $uri;

if ($uri !== "/" && is_file($file)) {
  return false;
}
if (preg_match("#^/api(/.*)?$#", $uri)) {
  require $root . "/api/index.php";
  return true;
}
if (preg_match("#^/install(/.*)?$#", $uri)) {
  $install = $root . "/install/index.php";
  if (is_file($install)) {
    require $install;
    return true;
  }
}
require $root . "/index.php";
return true;